#r "nuget: KiDev.Baikal, 0.3.2"
open KiDev.Baikal

Solution(__SOURCE_DIRECTORY__)
    |> AddProject(CS()
        |> OutputType Exe
        |> LangVersion 11
        |> TargetFramework "net6.0"
        |> Depedencies [
            NuGet "DSharpPlus" "4.3.0";
            NuGet "Microsoft.Extensions.Hosting" "7.0.1"
        ]
        |> None [
            Update "appsettings.json" |> CopyToOutput Always;
        ])
    |> run