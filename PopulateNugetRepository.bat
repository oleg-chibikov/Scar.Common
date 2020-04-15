@echo off

del /s /q "Nuget\*.*"
for /R "packages" %%I in (Scar.*nupkg) do copy /-y "%%I" Nuget