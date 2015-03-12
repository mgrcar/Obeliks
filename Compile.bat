set DISABLEOUTOFPROCTASKHOST=1

set msbuild=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
set msbuildCfgRelease=/p:Configuration=Release;TargetFrameworkVersion=v3.5
set msbuildCfgDebug=/p:Configuration=Debug;TargetFrameworkVersion=v3.5

%msbuild% PosTaggerTrain.sln  %msbuildCfgRelease%
%msbuild% PosTaggerTag.sln    %msbuildCfgRelease%
%msbuild% LemmatizerTrain.sln %msbuildCfgRelease%
%msbuild% PosTaggerTagGui.sln %msbuildCfgRelease%

del Compiled\*.* /s /q

copy ..\Latino\bin\Release\Latino.dll                Compiled\Release
copy LemmaSharp\bin\Release\LemmaSharp.dll           Compiled\Release
copy PosTagger\bin\Release\PosTagger.dll             Compiled\Release
copy PosTaggerTrain\bin\Release\PosTaggerTrain.exe   Compiled\Release
copy PosTaggerTag\bin\Release\PosTaggerTag.exe       Compiled\Release
copy LemmatizerTrain\bin\Release\LemmatizerTrain.exe Compiled\Release
copy PosTaggerTagGui\bin\Release\PosTaggerTagGui.exe Compiled\Release
copy PosTaggerTagGui\bin\Release\PosTaggerTagLib.dll Compiled\Release
copy LemmaSharp\bin\Release\Lzma#.dll                Compiled\Release

%msbuild% PosTaggerTrain.sln  %msbuildCfgDebug%
%msbuild% PosTaggerTag.sln    %msbuildCfgDebug%
%msbuild% LemmatizerTrain.sln %msbuildCfgDebug%
%msbuild% PosTaggerTagGui.sln %msbuildCfgDebug%

copy ..\Latino\bin\Debug\Latino.dll                Compiled\Debug
copy LemmaSharp\bin\Debug\LemmaSharp.dll           Compiled\Debug
copy PosTagger\bin\Debug\PosTagger.dll             Compiled\Debug
copy PosTaggerTrain\bin\Debug\PosTaggerTrain.exe   Compiled\Debug
copy PosTaggerTag\bin\Debug\PosTaggerTag.exe       Compiled\Debug
copy LemmatizerTrain\bin\Debug\LemmatizerTrain.exe Compiled\Debug
copy PosTaggerTagGui\bin\Debug\PosTaggerTagGui.exe Compiled\Debug
copy PosTaggerTagGui\bin\Debug\PosTaggerTagLib.dll Compiled\Debug
copy LemmaSharp\bin\Debug\Lzma#.dll                Compiled\Debug