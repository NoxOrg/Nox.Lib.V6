<# GooBenchmark.psm1 #>

class GooBenchmark {

    [string]$Name
    [Object]$Goo

    GooBenchmark( [Object]$goo ){
        $this.Name = "Benchmark"
        $this.Goo = $goo
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [void] WriteSummarySince( [string] $folder, [DateTime]$startTime ) {
        $oldColor=[Console]::ForegroundColor
        $summary = Import-Csv -Path (Get-ChildItem $folder -Filter '*.csv' `
        | Where-Object { $_.LastWriteTime -gt $startTime}).FullName `
        | Format-Table -Property Method,Mean,Error,StdDev,Max,Min,Ratio,Rank -AutoSize `
        | Out-String
        $summary.Split("`n") `
        | ForEach-Object {
            [Console]::ForegroundColor = `
                if($_.Contains("StdDev") -or $_.StartsWith("---")) {"Magenta"} 
                elseif ($_.Trim().EndsWith("1")) {"Cyan"} 
                else {"DarkGray"}
            if($_.Contains("_baseline")){ Write-Host "`n$($_.Trim())" } else { Write-Host $_.Trim() } 
        }
        [Console]::ForegroundColor=$oldColor    
    }
}

