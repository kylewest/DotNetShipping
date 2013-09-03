SET version=0
FOR /F "tokens=1" %%a IN ('git rev-list master') DO SET /A version+=1
tools\NAnt\NAnt.exe -buildfile:nant.build -D:version.revision=%version% %*