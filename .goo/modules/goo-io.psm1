<# GooIO.psm1 #>

class GooIO {

    [string]$Name
    [Object]$Goo

    GooIO( [Object]$goo ){
        $this.Name = "IO"
        $this.Goo = $goo
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [bool] EnsureRemoveFolder( [string]$folder ) {
        if(Test-Path -Path $folder) { 
            Remove-Item $folder -Force -Recurse 
            return $true
        } else {
            return $false
        }
    }

    [bool] EnsureFolderExists( [string]$folder ) {
        if(Test-Path -Path $folder) { 
            return $false
        } else {
            New-Item -ItemType Directory -Force -Path $folder | Out-Null
            return $true
        }
    }

    [void] JsonUpdateFile( [string]$fileName, [hashtable]$hash ) {
        $obj = Get-Content $fileName | ConvertFrom-Json
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

}
