# Unity MCP — setup for AI-assisted Unity development

This project includes the [CoplayDev unity-mcp](https://github.com/CoplayDev/unity-mcp)
package as a dev dependency. It lets an AI coding assistant (Claude Code,
Cursor, VS Code Copilot, etc.) talk directly to the Unity Editor — read the
scene hierarchy, edit components, run play mode, manage assets, etc. — via the
Model Context Protocol.

If you don't want to use it, you can just ignore it. The package only does
anything when you actively connect to it from an AI client.

## One-time setup (per machine)

### Step 1 — Open the project in Unity

The package URL is already in `Packages/manifest.json`. Unity downloads and
imports it automatically on first open. Wait for the Console to settle.

### Step 2 — Open the MCP for Unity window

Inside Unity:
- Top menu: **Window → MCP for Unity**.
- A panel appears with a list of supported AI clients (Claude Code, Cursor,
  Claude Desktop, etc.).

### Step 3 — Start the bridge

- In the MCP for Unity panel, click **Start Server**.
- It launches a local HTTP server on `localhost:8080`. You should see a green
  "Bridge Running" indicator.

### Step 4 — Register with your AI client

In the same panel:
- Select your client from the dropdown (e.g., **Claude Code**).
- Click **Configure** (or **Auto-Setup** in some versions).
- Wait for a green **Connected ✓** indicator.

If Claude Code isn't found:
- Make sure the `claude` CLI is installed and on PATH. On Windows:
  `irm https://claude.ai/install.ps1 | iex` in PowerShell, then `claude doctor`.
- Click **Choose Claude Install Location** in the MCP for Unity panel and
  point it at your installation.

### Step 5 — Restart Claude Code

The CLI loads MCP servers at startup. After Step 4 modifies the Claude config:
- **Close all Claude Code windows / shells.**
- Reopen.

You should now have Unity tools available in your next Claude Code session.
The agent can read your scene, modify GameObjects, run play mode, etc.

## Verifying it works

In a fresh Claude Code session, ask: *"What's the active scene in Unity, and
list the root GameObjects."*

The agent should call a tool like `unity_get_scene_info` or
`manage_gameobject` and return the actual current state of your editor. If it
just describes what it would do, the MCP connection isn't live.

## Troubleshooting

- **Bridge says "Stopped"** → click Start Server in the Unity panel.
- **Client says "Not connected"** → re-click Configure; restart Claude Code.
- **Port 8080 in use** → change the port in the MCP for Unity settings.
- **More issues** → see https://github.com/CoplayDev/unity-mcp/wiki/3.-Common-Setup-Problems

## What this is NOT

- This doesn't send your code to anyone unless you connect a client to it.
- It runs entirely on localhost.
- It only does what you tell it to do during a Claude/Cursor session.
- You can stop the bridge from the same panel any time.
