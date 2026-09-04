/**
 * phalanx-architecture.ts
 *
 * Loads, parses, and mutates `phalanx-architecture.yaml` so the rest of the
 * phalanx extension is driven by the file rather than a hardcoded mirror.
 *
 * Includes a small dependency-free YAML parser covering the subset the
 * architecture file uses: nested maps, lists (scalar + map items), inline
 * flow sequences `[a, b]`, folded scalars `>`, comments, and scalars.
 */

import * as fs from "node:fs";
import * as path from "node:path";

// ---------------------------------------------------------------------------
// Minimal YAML parser (subset)
// ---------------------------------------------------------------------------

interface Line {
  indent: number;
  content: string;
}

/** Strip a trailing comment (`#` at start-of-line or preceded by whitespace). */
function stripComment(line: string): string {
  for (let i = 0; i < line.length; i++) {
    const c = line[i];
    if (c === "#" && (i === 0 || line[i - 1] === " " || line[i - 1] === "\t")) {
      return line.slice(0, i);
    }
  }
  return line;
}

function tokenize(text: string): Line[] {
  const lines: Line[] = [];
  for (const raw of text.split("\n")) {
    const stripped = stripComment(raw);
    if (stripped.trim() === "") continue;
    const indent = stripped.length - stripped.trimStart().length;
    lines.push({ indent, content: stripped.trim() });
  }
  return lines;
}

function unquote(s: string): string {
  const t = s.trim();
  if (t.length >= 2 && ((t.startsWith('"') && t.endsWith('"')) || (t.startsWith("'") && t.endsWith("'")))) {
    return t.slice(1, -1);
  }
  return t;
}

function parseScalar(s: string): unknown {
  const t = s.trim();
  if (t === "" || t === "~" || t === "null" || t === "Null" || t === "NULL") return null;
  if (t === "true" || t === "True" || t === "TRUE") return true;
  if (t === "false" || t === "False" || t === "FALSE") return false;
  if (t === "{}") return {};
  if (t === "[]") return [];
  if (t.startsWith("[") && t.endsWith("]")) {
    const inner = t.slice(1, -1).trim();
    if (inner === "") return [];
    return inner.split(",").map((x) => unquote(x.trim()));
  }
  if (/^-?\d+$/.test(t)) return parseInt(t, 10);
  if (/^-?\d*\.\d+$/.test(t)) return parseFloat(t);
  return unquote(t);
}

type ParseResult = { value: unknown; next: number };

function parseBlockScalar(lines: Line[], idx: number, indent: number, folded: boolean): ParseResult {
  const parts: string[] = [];
  let i = idx;
  while (i < lines.length && lines[i].indent > indent) {
    parts.push(lines[i].content);
    i++;
  }
  return { value: folded ? parts.join(" ") : parts.join("\n"), next: i };
}

function parseMap(lines: Line[], idx: number, indent: number): ParseResult {
  const obj: Record<string, unknown> = {};
  let i = idx;
  while (i < lines.length) {
    const line = lines[i];
    if (line.indent < indent) break;
    if (line.indent > indent) {
      i++;
      continue;
    }
    if (line.content === "-" || line.content.startsWith("- ")) break; // belongs to enclosing list

    const m = line.content.match(/^([^:]+):(.*)$/);
    if (!m) {
      i++;
      continue;
    }
    const key = m[1].trim();
    const rest = m[2].trim();

    if (rest === "" || rest === ">" || rest === "|") {
      if (rest === ">" || rest === "|") {
        const r = parseBlockScalar(lines, i + 1, line.indent, rest === ">");
        obj[key] = r.value;
        i = r.next;
      } else if (i + 1 < lines.length && lines[i + 1].indent > indent) {
        const r = parseBlock(lines, i + 1, lines[i + 1].indent);
        obj[key] = r.value;
        i = r.next;
      } else {
        obj[key] = null;
        i++;
      }
    } else {
      obj[key] = parseScalar(rest);
      i++;
    }
  }
  return { value: obj, next: i };
}

function parseList(lines: Line[], idx: number, indent: number): ParseResult {
  const arr: unknown[] = [];
  let i = idx;
  while (i < lines.length) {
    const line = lines[i];
    if (line.indent < indent) break;
    if (line.indent > indent) {
      i++;
      continue;
    }
    if (line.content !== "-" && !line.content.startsWith("- ")) break;

    const rest = line.content === "-" ? "" : line.content.slice(2).trim();

    if (rest === "") {
      if (i + 1 < lines.length && lines[i + 1].indent > indent) {
        const r = parseBlock(lines, i + 1, lines[i + 1].indent);
        arr.push(r.value);
        i = r.next;
      } else {
        arr.push(null);
        i++;
      }
      continue;
    }

    // Could be a scalar list item or a map item ("- key: value").
    const keyMatch = rest.match(/^([^:]+):(.*)$/);
    if (!keyMatch) {
      arr.push(parseScalar(rest));
      i++;
      continue;
    }

    const item: Record<string, unknown> = {};
    const keyIndent = line.indent + 2; // column where "- " + key starts
    const firstKey = keyMatch[1].trim();
    const firstVal = keyMatch[2].trim();

    const assignValue = (k: string, v: string, lineIndent: number): number => {
      if (v === "" || v === ">" || v === "|") {
        if (v === ">" || v === "|") {
          const r = parseBlockScalar(lines, i + 1, lineIndent, v === ">");
          item[k] = r.value;
          return r.next;
        }
        if (i + 1 < lines.length && lines[i + 1].indent > lineIndent) {
          const r = parseBlock(lines, i + 1, lines[i + 1].indent);
          item[k] = r.value;
          return r.next;
        }
        item[k] = null;
        return i + 1;
      }
      item[k] = parseScalar(v);
      return i + 1;
    };

    i = assignValue(firstKey, firstVal, line.indent);

    // Subsequent keys of this map item, aligned under the first key.
    while (
      i < lines.length &&
      lines[i].indent === keyIndent &&
      lines[i].content !== "-" &&
      !lines[i].content.startsWith("- ")
    ) {
      const sub = lines[i];
      const sm = sub.content.match(/^([^:]+):(.*)$/);
      if (!sm) break;
      const k = sm[1].trim();
      const v = sm[2].trim();
      i = assignValue(k, v, sub.indent);
    }

    arr.push(item);
  }
  return { value: arr, next: i };
}

function parseBlock(lines: Line[], idx: number, indent: number): ParseResult {
  if (idx >= lines.length) return { value: null, next: idx };
  const line = lines[idx];
  if (line.indent < indent) return { value: null, next: idx };
  if (line.content === ">" || line.content === "|") {
    return parseBlockScalar(lines, idx + 1, line.indent, line.content === ">");
  }
  if (line.content === "-" || line.content.startsWith("- ")) {
    return parseList(lines, idx, line.indent);
  }
  return parseMap(lines, idx, line.indent);
}

export function parseYaml(text: string): unknown {
  const lines = tokenize(text);
  if (lines.length === 0) return null;
  return parseBlock(lines, 0, lines[0].indent).value;
}

// ---------------------------------------------------------------------------
// Typed model
// ---------------------------------------------------------------------------

export interface PhalanxRole {
  id: string;
  tier?: string;
  count?: unknown;
  reports_to?: unknown;
  dispatches?: string[];
  writes_to?: string[];
  interfaces?: string[];
  responsibility?: string;
  instances?: string[];
  accessed_by?: string[];
  consulted_by?: string[];
  tool?: unknown;
  path?: string;
  tracks?: string;
  trigger?: string;
  scope?: string;
  constraint?: string;
}

export interface PhalanxRule {
  id: string;
  statement: string;
}

export interface PhalanxModels {
  escalation?: string;
}

export interface PhalanxArchitecture {
  schemaVersion: unknown;
  roles: Record<string, PhalanxRole>;
  interfaces: Record<string, unknown>;
  deployment: Record<string, unknown>;
  rules: PhalanxRule[];
  models: PhalanxModels;
  raw: Record<string, unknown>;
  filePath: string;
}

function asRecord(v: unknown): Record<string, unknown> {
  return v && typeof v === "object" && !Array.isArray(v) ? (v as Record<string, unknown>) : {};
}

function asStringArray(v: unknown): string[] {
  if (Array.isArray(v)) return v.filter((x): x is string => typeof x === "string");
  return [];
}

function mapRole(id: string, v: unknown): PhalanxRole {
  const r = asRecord(v);
  return {
    id,
    tier: typeof r.tier === "string" ? r.tier : undefined,
    count: r.count,
    reports_to: r.reports_to,
    dispatches: asStringArray(r.dispatches),
    writes_to: asStringArray(r.writes_to),
    interfaces: asStringArray(r.interfaces),
    responsibility: typeof r.responsibility === "string" ? r.responsibility : undefined,
    instances: asStringArray(r.instances),
    accessed_by: asStringArray(r.accessed_by),
    consulted_by: asStringArray(r.consulted_by),
    tool: r.tool,
    path: typeof r.path === "string" ? r.path : undefined,
    tracks: typeof r.tracks === "string" ? r.tracks : undefined,
    trigger: typeof r.trigger === "string" ? r.trigger : undefined,
    scope: typeof r.scope === "string" ? r.scope : undefined,
    constraint: typeof r.constraint === "string" ? r.constraint : undefined,
  };
}

export function findArchitectureFile(cwd: string): string {
  const candidate = path.join(cwd, "phalanx-architecture.yaml");
  if (fs.existsSync(candidate)) return candidate;
  // fall back to a nested .pi location
  const alt = path.join(cwd, ".pi", "phalanx-architecture.yaml");
  if (fs.existsSync(alt)) return alt;
  return candidate; // return canonical path even if missing (caller checks)
}

/** A built-in fallback so the extension still works if the YAML is missing. */
export const DEFAULT_ARCHITECTURE_TEXT = `# fallback phalanx architecture (embedded)
schema_version: 1
roles:
  strategos:
    tier: command
    dispatches: [lochagos, psiloi]
  psiloi:
    tier: scout
    reports_to: strategos
    writes_to: [agora]
  lochagos:
    tier: coordinator
    reports_to: strategos
    instances: [work, research, build, verify]
  agora:
    tier: infrastructure
  oracle:
    tier: escalation
rules:
  - id: chain_of_command
    statement: a lochagos escalates failure to the strategos, never sideways
  - id: scout_first
    statement: probe with psiloi when the target location is unknown
  - id: shield_wall
    statement: retry at the narrowest scope once, then escalate
  - id: consult_the_oracle
    statement: if ambiguous or retries exhausted, ask the user
  - id: single_state
    statement: no private state; all reads and writes go through agora
models:
  escalation: ""
extend:
  general_principle: every new role declares exactly one reports_to
`;

function nonBlankString(v: unknown): string | undefined {
  return typeof v === "string" && v.trim() !== "" ? v.trim() : undefined;
}

export function parseArchitecture(text: string, filePath: string): PhalanxArchitecture {
  const raw = asRecord(parseYaml(text));
  const rolesRaw = asRecord(raw.roles);
  const roles: Record<string, PhalanxRole> = {};
  for (const [id, v] of Object.entries(rolesRaw)) {
    roles[id] = mapRole(id, v);
  }
  const rules: PhalanxRule[] = Array.isArray(raw.rules)
    ? raw.rules.map((r) => {
        const rr = asRecord(r);
        return {
          id: typeof rr.id === "string" ? rr.id : "",
          statement: typeof rr.statement === "string" ? rr.statement : "",
        };
      })
    : [];
  const modelsRaw = asRecord(raw.models);
  return {
    schemaVersion: raw.schema_version,
    roles,
    interfaces: asRecord(raw.interfaces),
    deployment: asRecord(raw.deployment),
    rules,
    models: { escalation: nonBlankString(modelsRaw.escalation) },
    raw,
    filePath,
  };
}

export function loadArchitecture(cwd: string): PhalanxArchitecture {
  const filePath = findArchitectureFile(cwd);
  if (!fs.existsSync(filePath)) {
    return parseArchitecture(DEFAULT_ARCHITECTURE_TEXT, filePath);
  }
  const text = fs.readFileSync(filePath, "utf-8");
  return parseArchitecture(text, filePath);
}

// ---------------------------------------------------------------------------
// Chain-of-command derivation
// ---------------------------------------------------------------------------

export function isLochagosAgent(name: string): boolean {
  return name === "lochagos" || name.startsWith("lochagos-");
}

/**
 * Which target roles a source role may dispatch, per the architecture.
 * strategos -> psiloi, lochagos (any domain).
 * lochagos / psiloi dispatch nothing further — they work directly and escalate up.
 */
export function mayDispatch(arch: PhalanxArchitecture, from: string, to: string): boolean {
  if (from === to) return false;
  const fromResolved = resolveRole(arch, from);
  if (!fromResolved) return false;
  const role = arch.roles[fromResolved.role];
  if (!role) return false;

  if (fromResolved.role === "strategos") {
    return to === "psiloi" || isLochagosAgent(to);
  }

  return false; // lochagos and psiloi dispatch nothing
}

/** Resolve a role name (possibly an instance) to `{ role, instance? }`. */
export function resolveRole(
  arch: PhalanxArchitecture,
  roleName: string,
): { role: string; instance?: string } | null {
  if (arch.roles[roleName]) return { role: roleName };

  if (isLochagosAgent(roleName)) {
    const inst = roleName === "lochagos" ? undefined : roleName.slice("lochagos-".length);
    if (!inst || arch.roles.lochagos?.instances?.includes(inst)) return { role: "lochagos", instance: inst };
  }
  return null;
}

