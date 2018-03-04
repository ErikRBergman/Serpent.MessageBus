cls

# First functions

function updateNodeVersion
{
    Param ([System.Xml.XmlElement] $versionNode, [string] $filename)

    if ($versionNode -eq $null)
    {
        return $null
    }

    $nodename = $versionNode.Name

     $oldVersion = $versionNode.InnerText
     $version = getUpdatedVersion $oldVersion
        
        if (-not $oldVersion.Equals($version))
        {
            $versionNode.InnerText = $version
            return "$filename : Updating $nodename from $oldVersion to $version"
        }
   
   return $null

}

function getUpdatedVersion
{
    Param ([string] $version)
    $versionArray = $version.Split(".")
    if ($versionArray.Length -eq 4)
    {
        $versionArray[3] = ([int]$versionArray[3]) + 1
        $version =  [string]::Join(".",$versionArray)

        return $version        
    }

    return $inputVerison
}


# Update version number


$updates = 0

$filter = "*.csproj"

echo "Scanning $filter..."

Get-ChildItem -Filter "$filter" | ForEach-Object { 
    
    $content = Get-Content $_.FullName 
    [xml]$xml = $content

    $wasUpdated = $false

    foreach ($propertyGroup in $xml.SelectNodes("//Project/PropertyGroup"))
    {
        $r = updateNodeVersion $propertyGroup.SelectSingleNode("Version") $_.FullName

        if ($r -ne $null)
        {
            echo $r
            $wasUpdated = $true
        }

        $r = updateNodeVersion $propertyGroup.SelectSingleNode("FileVersion") $_.FullName

        if ($r -ne $null)
        {
            echo $r
            $wasUpdated = $true
        }


        $r = updateNodeVersion $propertyGroup.SelectSingleNode("AssemblyVersion") $_.FullName

        if ($r -ne $null)
        {
            echo $r
            $wasUpdated = $true
        }


    }
    
    if ($wasUpdated -eq $true)
    { 
        $xml.Save($_.FullName);
        $updates = $updates + 1
    }

}


echo "$updates files updated"
