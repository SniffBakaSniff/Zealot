@echo off

wt ^
new-tab --title "Bot" cmd /k "cd /d %~dp0 && dotnet run --project Zealot.Bot" ^
; split-pane -V --title "Web" cmd /k "cd /d %~dp0 && dotnet run --project Zealot.Web"
