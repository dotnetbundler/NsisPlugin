@echo off
setlocal enabledelayedexpansion

:: 进入脚本所在目录
cd /d "%~dp0"

:: 定义插件项目所在目录
set "PLUGIN_DIR=%~dp0..\Plugins\"

:: 定义目标发布目录
set "DEST_DIR=%~dp0addplugin"
if not exist "%DEST_DIR%" mkdir "%DEST_DIR%"

:: 发布指定示例项目
set "TARGET_PROJECT=%~1"
if not "%TARGET_PROJECT%" == "" (
	:: 检查项目是否存在
	set "PROJ_PATH=%PLUGIN_DIR%%TARGET_PROJECT%"
	if not exist "!PROJ_PATH!\*.csproj" (
		echo [错误] 找不到项目: %TARGET_PROJECT%
		exit /b 1
	)
	echo [指定模式]
	call :PublishProject "!PROJ_PATH!" "%TARGET_PROJECT%"
	goto :end
)

:: 遍历所有示例项目
echo [遍历模式]
for /d %%i in ("%PLUGIN_DIR%*") do (
	if exist "%%i\*.csproj" (
		call :PublishProject "%%i" "%%~nxi"
	)
)
goto :end


:: --- 发布项目 ---
:PublishProject
echo ---------------------------------------
echo 正在发布: %~2
:: 发布是不生成XML文档的，且不包含调试信息，以减小体积
dotnet publish "%~1" -c Release -o "%DEST_DIR%" /p:GenerateDocumentationFile=false /p:DebugType=none /p:DebugSymbols=false
if %errorlevel% neq 0 echo [警告] 项目 %~2 发布失败！
goto :eof

:: --- 结束 ---
:end
echo ------------------[END]------------------
echo 完成，已发布至: %DEST_DIR%
exit /b 0
