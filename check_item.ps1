$conn = New-Object System.Data.SqlClient.SqlConnection("Server=(localdb)\MSSQLLocalDB;Database=HanhTrangLop1;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, Title, InteractionType, ContentJson FROM LearningItems WHERE Id = '05c2e491-51ef-4bac-a430-8378f233c663'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "TITLE: $($reader['Title'])"
    Write-Output "TYPE: $($reader['InteractionType'])"
    Write-Output "CONTENT: $($reader['ContentJson'])"
}
$conn.Close()
