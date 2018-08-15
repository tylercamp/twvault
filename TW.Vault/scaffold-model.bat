@echo off
set modeldir=Scaffold
echo modeldir=%modeldir%
echo running scaffolding

REM args:
REM   -o Output directory
REM   -c Context name
REM   -f Force
dotnet ef dbcontext scaffold "Host=192.168.1.250; Port=22342; Database=vault; Username=twu_vault; Password=!!TWV@ult4Us??" Npgsql.EntityFrameworkCore.PostgreSQL -o %modeldir% -c VaultContext --schema tw --schema tw_provided --schema security -f
pause 