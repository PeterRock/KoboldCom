# Changelog

## 2.0.0

Breaking release: the library now targets modern .NET instead of .NET Framework 3.5.

- SDK-style projects; library TFM is `net8.0` (LTS).
- Serial I/O comes from the stable NuGet package `System.IO.Ports` (not the in-box Framework assembly).
- Demo is `net8.0-windows` WinForms and references the 2.x library.
- Assembly / file / package version is **2.0.0**.
- Public C# API and Chinese XML docs are preserved; this modernization does not rewrite protocol logic.
- Includes already-merged 1.x work: TextProtocolAnalyzer `CheckData` (PR #4) and Hex `CheckLength` / `CheckData16` plus Handshake.None RTS/DTR (PR #5).

**.NET Framework 3.5 consumers cannot use 2.x.** Stay on the `v1.1.0` tag or the `netfx-1.x` branch.

## 1.1.0

Tagged **.NET Framework 3.5** snapshot when 2.0 was planned (`v1.1.0`).

- Optional `CheckData` support on `TextProtocolAnalyzer` (PR #4).
- Console checks: `verify/TextProtocolAnalyzerCheckData`.

PR #5 (hex 2-byte checksum / Handshake.None RTS/DTR) landed on Framework master after this tag. Those commits remain on `netfx-1.x` and are included in 2.0.0.

## 1.0.0

Original tagged Framework 3.5 release (`v1.0.0`).
