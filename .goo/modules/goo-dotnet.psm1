<# GooDotnet.psm1 #>

class GooDotnet {

    [string]$Name
    [Object]$Goo

    GooDotnet( [Object]$goo ){
        $this.Name = "Dotnet"
        $this.Goo = $goo
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [string] EnsureSecretsFileExists( [string]$projectFile ) {
        $id = ([xml](Get-Content $projectFile)).Project.PropertyGroup.UserSecretsId
        $fileName = "$Env:APPDATA\Microsoft\UserSecrets\$id\secrets.json"
        if( -not (Test-Path -Path $fileName) ) {
            Set-Content -Path $fileName -Value "{}"
        }
        return $fileName
    }

    [void] SecretsSet( [string]$projectFile, [hashtable]$hash ) {
        $fileName = $this.EnsureSecretsFileExists($projectFile)
        $obj = Get-Content -Path $fileName | ConvertFrom-Json
        foreach( $kvp in $hash.GetEnumerator() ) {
            $propName = $kvp.name
            $value = $kvp.Value
            if( [bool]($obj.PSobject.Properties.name -match $propName) ) {
                $obj.$propName = $value
            } else {
                Add-Member -InputObject $obj -Name $propName -Value $value -MemberType NoteProperty
            }
        }
        $obj | ConvertTo-Json | set-content $fileName
    }

    [void] SecretsRemove( [string]$projectFile, [string[]]$list ) {
        $fileName = $this.EnsureSecretsFileExists($projectFile)
        $obj = Get-Content -Path $fileName | ConvertFrom-Json
        foreach( $propName in $list ) {
            if( [bool]($obj.PSobject.Properties.name -match $propName) ) {
                $obj.PSObject.Properties.Remove($propName)
            }
        }
        $obj | ConvertTo-Json | set-content $fileName
    }

}
