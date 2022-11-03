<# GooSql.psm1 #>

class GooSql {

    [string]$Name
    [Object]$Goo

    GooSql( [Object]$goo ){
        $this.Name = "Sql"
        $this.Goo = $goo
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [void] CheckModule()
    {
        if( -not ((Get-Module -ListAvailable -Name "SqlServer"))) {
            $this.Goo.Error("Module not found. Please run 'Install-Module -Name SqlServer' as admin")
        }
    }


    [void] Query( [string]$connString, [string]$sql ){
        $this.CheckModule()
        Invoke-Sqlcmd -ConnectionString $connString -Query $sql | Format-Table -AutoSize | Out-Host
        if( -not $? ) { $this.Goo.Error( "Sql error." ) }
    }

    [bool] TestConnection([string]$connString) {
        $this.CheckModule()
        try {
            $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connString
            $sqlConnection.Open()
            Invoke-Sqlcmd -ConnectionString $connString -Query "SELECT 1" | Out-Null
            return $true
        } catch {
            return $false
        } finally {
            $sqlConnection.Close()
        }
    }

    [bool] WaitForConnection([string]$connString, [int]$seconds) {
        $this.CheckModule()
        $ret = $false
        for($i = $seconds; $i -gt 0; $i--) {
            if($this.TestConnection($connString)) {
                $ret = $true
                break
            }
            $this.Goo.Sleep(1)
        }
        return $ret
    }
}
