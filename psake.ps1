properties {
    $global:config = "debug"
 
    $tag = $(git tag -l --points-at HEAD)
    $commitHash = $(git rev-parse --short HEAD)
 
    $base_dir = resolve-path .
    $source_dir = "$base_dir\src"
    $docs_dir = "$base_dir\docs"
    $arcus_dir = "$source_dir\Arcus"
    $arcus_tests_dir = "$source_dir\Arcus.Tests"
 
    $arcus_sln = "$source_dir\Arcus.sln"
    $arcus_csproj = "$arcus_dir\Arcus.csproj"
}
 
task release -depends build_src {
    $global:config = "release"
}
 
task default -depends build_debug
 
task build_release -depends release, build_src, build_docs
 
task build_debug -depends build_src, build_docs, test
 
task build_src -depends clean {
    echo "building $config..."
    echo "Tag: $tag"
    echo "CommitHash: $commitHash"
     
    exec { dotnet --version }
    exec { dotnet --info }
 
    exec { dotnet build $arcus_sln -c $config }
}
 
task build_docs -depends clean_docs {
 
    exec { cmd.exe /c $docs_dir/make.bat html }
    
}
 
task test {
    exec { & dotnet test $arcus_sln -c $config --no-build --no-restore }
}
 
task clean_docs {
    rd "$docs_dir\_build" -recurse -force  -ErrorAction SilentlyContinue | out-null
}
 
task clean {
    rd "$arcus_dir\bin" -recurse -force  -ErrorAction SilentlyContinue | out-null
    rd "$arcus_dir\obj" -recurse -force  -ErrorAction SilentlyContinue | out-null
 
    rd "$arcus_tests_dir\bin" -recurse -force  -ErrorAction SilentlyContinue | out-null
    rd "$arcus_tests_dir\obj" -recurse -force  -ErrorAction SilentlyContinue | out-null

}
 
task pack -depends release, clean {

    echo "releasing $config..."
    echo "Tag: $tag"
    echo "CommitHash: $commitHash"

    dotnet pack $arcus_csproj -c $config --no-restore --include-symbols --include-source --verbosity m
}