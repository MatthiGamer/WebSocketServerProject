@echo off
start "%~dp0"WebSocketServer\bin\Release\net6.0\WebSocketServer.exe "%~dp0"\WebSocketServerProject\WebSocketServer\bin\Release\net6.0\WebSocketServer.exe
start cmd /c "cd "%~dp0"\websocketclient & serve -s build"