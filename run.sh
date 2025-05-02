#!/bin/bash

dotnet build AdventureS25.sln

dotnet run --project="AdventureS25/AdventureS25.csproj"

#!/bin/bash

# Publish the project as a single executable
#dotnet publish ./AdventureS25-master/AdventureS25/AdventureS25.csproj \
    #-c Release \
    #-r win-x64 \
    #--self-contained true \
    #-p:PublishSingleFile=true \
    #-p:AssemblyName=PalQuest \
    #-o ./publish

#echo "Executable created at ./publish/PalQuest.exe"