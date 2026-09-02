/**
 * agora.ts — the phalanx shared memory and message bus.
 *
 * Implements the `single_state` rule: no agent holds private state; all reads
 * and writes go through agora. Backed by a JSON file at `.pi/phalanx/agora.json`
 * so both the main session and dispatched subagent processes can reach it.
 *
 * Mutations are serialized through an in-process queue so parallel tool calls
 * cannot interleave read-modify-write sequences.
 */

import * as fs from "node:fs";
import * as path from "node:path";

export interface AgoraMessage {
  id: string;
  from: string;
  to: string; // role name or "*" for broadcast
  content: string;
  ts: number;
  read: boolean;
}

export interface AgoraLogEntry {
  ts: number;
  event: string;
  role?: string;
  detail?: string;
}

export interface AgoraState {
  keys: Record<string, unknown>;
  messages: AgoraMessage[];
  log: AgoraLogEntry[];
  attempts: Record<string, number>;
}

export function emptyAgora(): AgoraState {
  return { keys: {}, messages: [], log: [], attempts: {} };
}

function makeId(prefix: string): string {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

export class AgoraStore {
  private state: AgoraState;
  private readonly filePath: string;
  private queue: Promise<unknown> = Promise.resolve();

  constructor(cwd: string) {
    this.filePath = path.join(cwd, ".pi", "phalanx", "agora.json");
    this.state = this.load();
  }

  get path(): string {
    return this.filePath;
  }

  private load(): AgoraState {
    try {
      if (fs.existsSync(this.filePath)) {
        const raw = JSON.parse(fs.readFileSync(this.filePath, "utf-8"));
        return { ...emptyAgora(), ...raw };
      }
    } catch {
      /* corrupt or unreadable -> start fresh */
    }
    return emptyAgora();
  }

  private persist(): void {
    const dir = path.dirname(this.filePath);
    fs.mkdirSync(dir, { recursive: true });
    const tmp = `${this.filePath}.tmp`;
    fs.writeFileSync(tmp, JSON.stringify(this.state, null, 2), "utf-8");
    fs.renameSync(tmp, this.filePath);
  }

  /** Serialize a mutation so concurrent tool calls cannot interleave. */
  private mutate<T>(fn: () => T): Promise<T> {
    const run = () => {
      const result = fn();
      this.persist();
      return result;
    };
    const p = this.queue.then(run, run);
    this.queue = p.catch(() => {});
    return p;
  }

  snapshot(): AgoraState {
    return JSON.parse(JSON.stringify(this.state)) as AgoraState;
  }

  // ---- key/value store -----------------------------------------------------

  get(key: string): unknown {
    return this.state.keys[key];
  }

  put(key: string, value: unknown): Promise<void> {
    return this.mutate(() => {
      this.state.keys[key] = value;
    });
  }

  del(key: string): Promise<boolean> {
    return this.mutate(() => {
      const had = Object.prototype.hasOwnProperty.call(this.state.keys, key);
      delete this.state.keys[key];
      return had;
    });
  }

  listKeys(): string[] {
    return Object.keys(this.state.keys);
  }

  // ---- message bus ---------------------------------------------------------

  post(from: string, to: string, content: string): Promise<AgoraMessage> {
    return this.mutate(() => {
      const msg: AgoraMessage = {
        id: makeId("msg"),
        from,
        to,
        content,
        ts: Date.now(),
        read: false,
      };
      this.state.messages.push(msg);
      return msg;
    });
  }

  inbox(role: string): AgoraMessage[] {
    return this.state.messages.filter((m) => m.to === role || m.to === "*");
  }

  markRead(id: string): Promise<boolean> {
    return this.mutate(() => {
      const msg = this.state.messages.find((m) => m.id === id);
      if (!msg) return false;
      msg.read = true;
      return true;
    });
  }

  // ---- event log -----------------------------------------------------------

  log(event: string, role?: string, detail?: string): Promise<void> {
    return this.mutate(() => {
      this.state.log.push({ ts: Date.now(), event, role, detail });
      if (this.state.log.length > 500) {
        this.state.log = this.state.log.slice(-500);
      }
    });
  }

  recentLog(n = 20): AgoraLogEntry[] {
    return this.state.log.slice(-n);
  }

  // ---- shield_wall retry tracking ------------------------------------------

  attemptsFor(scope: string): number {
    return this.state.attempts[scope] ?? 0;
  }

  bumpAttempt(scope: string): Promise<number> {
    return this.mutate(() => {
      const n = (this.state.attempts[scope] ?? 0) + 1;
      this.state.attempts[scope] = n;
      return n;
    });
  }

  resetAttempt(scope: string): Promise<void> {
    return this.mutate(() => {
      delete this.state.attempts[scope];
    });
  }

  // ---- reset ---------------------------------------------------------------

  clear(): Promise<void> {
    return this.mutate(() => {
      this.state = emptyAgora();
    });
  }
}
