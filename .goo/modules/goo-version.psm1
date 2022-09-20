<# GooVersion.psm1 #>

class GooVersion {

    [string]$Name
    [Object]$Goo
    [string]$Current

    hidden [string]$_defaultVersionInfoFile = '.\.goo\goo.version'
    hidden [string]$_latestVersionInfoFile = '.\.goo\goo.latest.version'
    hidden [string]$_versionPattern = '(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)'
    hidden [string]$_projectReleaseUri = 'https://github.com/andresharpe/goo/releases/download/latest/publish.zip'
    hidden [string]$_projectLatestInfoUri ='https://api.github.com/repos/andresharpe/goo/releases/latest'

    GooVersion( [Object]$goo ){
        $this.Name = "Version"
        $this.Goo = $goo
        $this.Current = $this.CurrentVersion($this._defaultVersionInfoFile)
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [string] Bump([string]$versionInfoFile, [string]$part)
    {

        $version = $this.Get($versionInfoFile)

        [string]$element = 'Build'

        switch -wildcard ($part)
        {
            "ma*"   { $element = 'Major' }
            "mi*"   { $element = 'Minor' }
            "p*"    { $element = 'Patch' }
            "b*"    { $element = 'Build' }
            default { $this.Goo.Error('Version part parameter should be: minor, major, patch or build!')}
        }

        $version.$element = (([int]$version.$element)+1)

        switch ($element)
        {
            'Major' { $version.Minor = 0; $version.Patch = 0 }
            'Minor' { $version.Patch = 0 }
        }

        $this.Set($version)

        return "$($version.Major).$($version.Minor).$($version.Patch) (build $($version.Build))"

    }

    [string] CurrentVersion([string]$versionInfoFile)
    {
        $version = $this.Get($versionInfoFile)
        return "$($version.Major).$($version.Minor).$($version.Patch) (build $($version.Build))"
    }
    
    [string] LatestVersion([string]$versionInfoFile)
    {
        if(-not $this.Goo.Network.IsOnline() ) {
            return $this.Current
        }

        if( -not (Test-Path variable:global:GOO_LATEST_VERSION)) {
            $global:GOO_LATEST_VERSION = (Invoke-WebRequest -Method Get -Uri $this._projectLatestInfoUri | ConvertFrom-Json).Name.Trim('v')
        }

        return $global:GOO_LATEST_VERSION
    }
    
    [string] BumpBuild([string]$versionInfoFile)
    {
        return $this.Bump($versionInfoFile,'Build')
    }

    [string] BumpPatch([string]$versionInfoFile)
    {
        return $this.Bump($versionInfoFile,'Patch')
    }

    [string] BumpMinor([string]$versionInfoFile)
    {
        return $this.Bump($versionInfoFile,'Minor')
    }

    [string] BumpMajor([string]$versionInfoFile)
    {
        return $this.Bump($versionInfoFile,'Major')
    }

    [object] Get([string]$versionInfoFile)
    {
    	
    	$versionPattern = $this._versionPattern

        if(Test-Path $versionInfoFile) {
            $versionInfo = Get-ChildItem $versionInfoFile 
        }
        else {
            $versionInfo = Get-ChildItem -Recurse | 
                Where-Object {$_.Name -eq $versionInfoFile} | 
                Select-Object -First 1
        }

    	if(!$versionInfo)
    	{
    		$this.Goo.Error("Could not find version info file")
    	}

    	$matchedLine = Get-Content $versionInfo.FullName |
    	    Where-Object { $_ -match $versionPattern } |
    		Select-Object -First 1

    	if(!$matchedLine)
    	{
    		$this.Goo.Error("Could not find line containing version in version info file")
    	}					   

    	$major, $minor, $patch, $build = 
            ([regex]$versionPattern).matches($matchedLine) |
    		ForEach-Object {$_.Groups } | 
    		Select-Object -Skip 1

    	return New-Object PSObject -Property @{
            FileName = $versionInfo.FullName
    		Major = $major.Value
    		Minor = $minor.Value
    		Patch = $patch.Value
    		Build = $build.Value
    	}
    }

    [void] Set([object]$version)
    {
    	$versionPattern = $this._versionPattern
        $newVersion = "$($version.Major).$($version.Minor).$($version.Patch).$($version.Build)"

  		$currentFile = $version.FileName
        $tempFile = ("$currentFile.tmp")
  		
        Get-Content $currentFile | ForEach-Object {
  			ForEach-Object { $_ -Replace $versionPattern, $newVersion }
   		} | Set-Content $tempFile
   		
        Remove-Item $currentFile
   		
        Rename-Item $tempFile $currentFile
    }

    # User callable commands

    [void] GooRelease()
    {
        $this.Goo.IO.EnsureFolderExists('.\dist')
        Compress-Archive -Path '.\.goo' -CompressionLevel Optimal -DestinationPath '.\dist\publish.zip' -Force

        $this.Goo.Command.RunExternal('gh','release delete latest --yes' )

        $ver = $this.Get($this._defaultVersionInfoFile)
        $this.Goo.Command.RunExternal('gh','release create latest --notes "latest release" --title "v'+"$($ver.Major).$($ver.Minor).$($ver.Patch)"+'" .\dist\publish.zip' )
        $this.Goo.StopIfError("Failed to create latest release on GitHub (gh cli)")

        $this.Goo.Command.RunExternal('gh', 'release list --limit 3')
    }

    [void] GooUpdate()
    {
        $this.GooGetVersion();
        $this.Goo.Console.WriteInfo("Updating...")
        
        $tempFile = New-TemporaryFile
        Invoke-WebRequest -Method Get -Uri $this._projectReleaseUri -OutFile $tempFile
        Expand-Archive -Path $tempFile -DestinationPath '.' -Force
        Remove-Item $tempFile -Force

        $this.GooGetVersion();
    }

    [void] GooBumpVersion([string] $part)
    {
        $newVersion = $this.Bump( $this._defaultVersionInfoFile, $part )
        $this.Goo.Console.WriteInfo("Bumped goo $part version. The new version is $newVersion")
    }

    [void] GooGetVersion() {
        $this.GooGetVersion($false)
    }

    [void] GooGetVersion([bool]$forceShow)
    {
        $currentVersion = $this.Current
        $latestVersion = $this.LatestVersion($this._defaultVersionInfoFile)
        $isOutOfDate = (-not $currentVersion.StartsWith($latestVersion))

        if( $forceShow -or $isOutOfDate){
            $this.Goo.Console.WriteInfo("The current version of goo is $currentVersion. The latest version of goo is ($latestVersion)!")
            if( $isOutOfDate ){
                $this.Goo.Console.WriteInfo("Run 'goo goo-update' to update your project.")
            }
        }
    }

}
