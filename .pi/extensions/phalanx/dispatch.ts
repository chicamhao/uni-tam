/**
 * dispatch.ts — spawns an isolated `pi` subprocess to run a single phalanx role.
 *
 * Each role agent runs in its own process with an isolated context window,
 * restricted tools (from agent frontmatter), and a role system prompt. Output
 * is captured from `--mode json` event lines.
 */

import { spawn } from "node:child_process";
import * as fs from "node:fs";
import * as os from "node:os";
import * as path from "node:path";

export interface DispatchResult {
  agentName: string;
  task: string;
  exitCode: number;
  output: string;
  stderr: string;
  isError: boolean;
  stopReason?: string;
  errorMessage?: string;
  turns: number;
  model?: string;
  aborted: boolean;
}

export interface DispatchOptions {
  cwd: string;
  agentName: string;
  systemPrompt: string;
  task: string;
  tools?: string[];
  model?: string;
  signal?: AbortSignal;
  onUpdate?: (partial: string) => void;
}

function getPiInvocation(args: string[]): { command: string; args: string[] } {
  const currentScript = process.argv[1];
  const isBunVirtualScript = currentScript?.startsWith("/$bunfs/root/");
  if (currentScript && !isBunVirtualScript && fs.existsSync(currentScript)) {
    return { command: process.execPath, args: [currentScript, ...args] };
  }

  const execName = path.basename(process.execPath).toLowerCase();
  const isGenericRuntime = /^(node|bun)(\.exe)?$/.test(execName);
  if (!isGenericRuntime) {
    return { command: process.execPath, args };
  }
  return { command: "pi", args };
}

interface JsonMessage {
  role?: string;
  content?: Array<{ type: string; text?: string }>;
  stopReason?: string;
  errorMessage?: string;
  model?: string;
  usage?: { totalTokens?: number };
}

function finalText(msg: JsonMessage | undefined): string {
  if (!msg?.content) return "";
  for (const part of msg.content) {
    if (part.type === "text" && part.text) return part.text;
  }
  return "";
}

export async function runSubagent(opts: DispatchOptions): Promise<DispatchResult> {
  const { cwd, agentName, systemPrompt, task, tools, model, signal, onUpdate } = opts;

  const result: DispatchResult = {
    agentName,
    task,
    exitCode: 0,
    output: "",
    stderr: "",
    isError: false,
    turns: 0,
    aborted: false,
  };

  const args: string[] = ["--mode", "json", "-p", "--no-session", "--no-extensions"];
  if (model) args.push("--model", model);
  if (tools && tools.length > 0) args.push("--tools", tools.join(","));

  let tmpDir: string | null = null;
  let tmpPromptPath: string | null = null;

  try {
    if (systemPrompt.trim()) {
      tmpDir = await fs.promises.mkdtemp(path.join(os.tmpdir(), "pi-phalanx-"));
      tmpPromptPath = path.join(tmpDir, `prompt-${agentName}.md`);
      await fs.promises.writeFile(tmpPromptPath, systemPrompt, { encoding: "utf-8", mode: 0o600 });
      args.push("--append-system-prompt", tmpPromptPath);
    }

    args.push(`Task: ${task}`);

    const exitCode = await new Promise<number>((resolve) => {
      const invocation = getPiInvocation(args);
      const proc = spawn(invocation.command, invocation.args, {
        cwd,
        shell: false,
        stdio: ["ignore", "pipe", "pipe"],
      });

      let buffer = "";
      let lastAssistant: JsonMessage | undefined;

      const processLine = (line: string) => {
        if (!line.trim()) return;
        let event: any;
        try {
          event = JSON.parse(line);
        } catch {
          return;
        }
        if (event.type === "message_end" && event.message) {
          const msg = event.message as JsonMessage;
          if (msg.role === "assistant") {
            lastAssistant = msg;
            result.turns++;
            if (msg.model) result.model = msg.model;
            if (msg.stopReason) result.stopReason = msg.stopReason;
            if (msg.errorMessage) result.errorMessage = msg.errorMessage;
            result.output = finalText(msg);
            if (onUpdate) onUpdate(result.output);
          }
        }
      };

      proc.stdout.on("data", (data) => {
        buffer += data.toString();
        const lines = buffer.split("\n");
        buffer = lines.pop() || "";
        for (const line of lines) processLine(line);
      });

      proc.stderr.on("data", (data) => {
        result.stderr += data.toString();
      });

      proc.on("close", (code) => {
        if (buffer.trim()) processLine(buffer);
        resolve(code ?? 0);
      });

      proc.on("error", () => resolve(1));

      if (signal) {
        const killProc = () => {
          result.aborted = true;
          proc.kill("SIGTERM");
          setTimeout(() => {
            if (!proc.killed) proc.kill("SIGKILL");
          }, 5000);
        };
        if (signal.aborted) killProc();
        else signal.addEventListener("abort", killProc, { once: true });
      }
    });

    result.exitCode = exitCode;
    if (result.aborted) {
      result.isError = true;
      result.stopReason = "aborted";
    } else if (
      exitCode !== 0 ||
      result.stopReason === "error" ||
      result.stopReason === "aborted" ||
      result.errorMessage
    ) {
      result.isError = true;
      if (!result.output) result.output = result.errorMessage || result.stderr || "(no output)";
    }

    return result;
  } finally {
    if (tmpPromptPath) {
      try {
        fs.unlinkSync(tmpPromptPath);
      } catch {
        /* ignore */
      }
    }
    if (tmpDir) {
      try {
        fs.rmdirSync(tmpDir);
      } catch {
        /* ignore */
      }
    }
  }
}
