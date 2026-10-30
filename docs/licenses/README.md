# Notice sources

These upstream files are preserved with trailing whitespace normalized and copied into the portable package:

- `Microsoft.Data.Sqlite-LICENSE.txt`: https://raw.githubusercontent.com/dotnet/efcore/v10.0.13/LICENSE.txt
- `SQLitePCLRaw-LICENSE.txt` and `SQLitePCLRaw-NOTICE.txt`: https://github.com/ericsink/SQLitePCL.raw/tree/ca835d21508bff43121c65081035840ac5006c4c (the published v2.1.12 source notices; the restored 2.1.13 package metadata declares Apache-2.0 and Copyright 2014-2024 SourceGear, LLC).

The complete upstream SQLitePCLRaw notice file also describes optional components, such as SQLCipher, that are not included in Homeryn's `e_sqlite3` distribution. Runtime-pack license/notice files are copied from the exact restored .NET packs during packaging; their versions are recorded in `DEPENDENCIES.txt`.
