.\venv\Scripts\activate
$job1 = databricks jobs create --json-file .\streaming_job.json | ConvertFrom-Json
Write-Host "Executing Job " $job1.job_id
databricks jobs run-now --job-id $job1.job_id

