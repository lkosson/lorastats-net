@echo off
git log -1 --pretty=format:%%as/%%h > version.txt
set /p version=<version.txt
del version.txt
echo public partial class GeneratedVersion { public const string VALUE = ^"%version%^"; }> GeneratedVersion.cs
