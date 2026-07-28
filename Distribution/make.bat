@echo off
setlocal EnableDelayedExpansion

set "VS_PATH="

for /d %%D in ("C:\Program Files\Microsoft Visual Studio\2022\*") do (
    if exist "%%D\MSBuild\Current\Bin\MSBuild.exe" (
        set "VS_PATH=%%D"
        goto :found
    )
)

:found
if not defined VS_PATH (
    echo Visual Studio not found
    exit /b 1
)

echo Visual Studio trovato:
echo %VS_PATH%

set MSBUILD="%VS_PATH%\MSBuild\Current\Bin\amd64\MSBuild.exe"

echo MSBuild:
echo %MSBUILD%

set SAVE=%cd%
cd ..
git clean -fdx
dotnet nuget locals all --clear
%MSBUILD% SeaMath.sln /t:Restore
%MSBUILD% SeaMath.sln /t:Build /p:Configuration=Release /p:Platform=x64

cd %SAVE%


endlocal


rem "C:\Program Files\Microsoft Visual Studio\2022\MSBuild\Current\Bin\amd64\MSBuild.exe"