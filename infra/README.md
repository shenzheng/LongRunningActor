
```bash
curl.exe -G "http://localhost:3100/loki/api/v1/query_range" --data-urlencode 'query={service_name="LongRunningActor"}' --data-urlencode 'limit=20' --data-urlencode 'start='$(($(date +%s%N) - 5*60*1_000_000_000))
```

```bash
curl.exe -G "http://localhost:3100/loki/api/v1/query_range" ^
 --data-urlencode "query={service_name!=\"\"}" ^
 --data-urlencode "limit=20"
```
