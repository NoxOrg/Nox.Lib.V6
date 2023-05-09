<# GooNetwork.psm1 #>

class GooNetwork {

    [string]$Name
    [Object]$Goo

    GooNetwork( [Object]$goo ){
        $this.Name = "Network"
        $this.Goo = $goo
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [void] EnsureConnectionTo( [string]$deviceAddress ) {
        if (-not (Test-Connection -ComputerName $deviceAddress -Count 1 -ErrorAction 0 -Quiet) ) {
            $this.Goo.Error( "Please connect to the VPN before running an init." )
        }
    }

    [bool] IsOnlineOld() {
        return $null -ne (Get-NetRoute | Where-Object DestinationPrefix -eq '0.0.0.0/0' | Get-NetIPInterface | Where-Object ConnectionState -eq 'Connected')
    }

    [bool] IsOnline() {
        return (Test-Connection 8.8.8.8 -Count 1 -Quiet)
    }

}
