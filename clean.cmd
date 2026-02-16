@echo off
  

set "folder_bin=bin"
set "folder_obj=obj"
set "folder_logs=logs"


for /r %%i in (%folder_obj%) do (
	if exist "%%i" rd /s /q "%%i"
)

for /r %%i in (%folder_bin%) do (
	if exist "%%i" rd /s /q "%%i"
)

for /r %%i in (%folder_logs%) do (
	if exist "%%i" rd /s /q "%%i"
)


pause