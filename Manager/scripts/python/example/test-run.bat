@echo off
set MY_IPC_SERVER_PORT=40010
set MY_IPC_CLIENT_PORT=40011
set NODEJS_INCLUDE_PATH=..\..\lib\nodejs
set SCRIPTS_MANAGER_TEST_MODE=true
node server.js
pause