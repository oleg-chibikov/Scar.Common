@echo off

del /s /q "Nuget\*.*"
for /R "%userprofile%\.nuget\packages" %%I in (Scar.*nupkg) do copy /-y "%%I" Nuget