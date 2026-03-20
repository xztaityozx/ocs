# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ocs (oneliner csharp)** — AWK風の構文でC#をワンライナー実行するCLIツール。

```bash
seq 10 | ocs 'BEGIN{var sum=0}{sum+=int.Parse(F0)}END{Console.WriteLine(sum)}'
# => 55
```

## Commands

```bash
# ビルド (net8.0, Release)
make publish

# インストール ($HOME/.local/bin)
make install

# テスト実行
cd Test && dotnet test --verbosity normal -c Debug

# 単一テストクラスを実行
cd Test && dotnet test --verbosity normal -c Debug --filter "FullyQualifiedName~GlobalTest"
```

## Architecture

```
CLI Input (Options.cs)
  → ConfigFactory (JSON設定読み込み: ~/.config/ocs/setting.json)
  → Parser.cs (スクリプト解析 → OcsScript レコード)
  → RenderService.cs (Scriban テンプレート → C# Runner クラス生成)
  → CompileService.cs (Roslyn でメモリ内コンパイル)
  → IRunner 実装を動的インスタンス化
  → Global.cs (行処理ループ、AWK変数管理)
```

### 主要コンポーネント

| モジュール | 役割 |
|-----------|------|
| `Lib/Parser.cs` | `BEGIN{}`/`END{}`/パターンブロックの解析。括弧・文字列リテラルの再帰的マッチング |
| `Global/Global.cs` | `F0`(行全体), `F[]`(フィールド配列), `NR`(行番号), `NF`(フィールド数), `i()`, `d()`, `print()` などのAWK互換変数・関数 |
| `Service/Template/RenderService.cs` | Scriban テンプレートで Runner クラスのC#ソースを生成 |
| `Service/Compile/CompileService.cs` | Microsoft.CodeAnalysis (Roslyn) でC#をメモリコンパイルし IRunner を返す |
| `Cli/Options.cs` | CommandLineParser による引数処理 (`-d`, `-D`, `-r`, `-g`, `-U`, `-R`, `--print-generated` など) |
| `Lib/Config/` | `Config.cs` (設定モデル), `ConfigFactory.cs` (JSONファイル読み込み) |

### OcsScript の構造

`Parser` が入力スクリプトを解析し `OcsScript` レコードを生成する。パターン部分が空の場合は全行マッチ、`BEGIN`/`END` は対応するフェーズで実行される。生成されたC#コードは `RenderService` でクラスに埋め込まれ Roslyn でコンパイルされる。

## Key Technologies

- **.NET 8.0** / C#
- **Roslyn** (`Microsoft.CodeAnalysis.CSharp` 4.10.0) — 動的コンパイル
- **Scriban** 5.10.0 — テンプレートエンジン
- **CommandLineParser** 2.9.1 — CLI引数
- **Microsoft.Extensions.*** 8.0.0 — DI, ロギング, 設定
- **xUnit** 2.8.1 + **Coverlet** — テスト・カバレッジ
