import json, os, glob

for f in sorted(glob.glob('seeddata/raw/*.json')):
    with open(f, encoding='utf-8') as fp:
        data = json.load(fp)
    if isinstance(data, list):
        row = data[0] if data else {}
        count = len(data)
    elif isinstance(data, dict):
        count = 'dict'
        row = {}
        for k in ('jobs', 'results', 'data', 'items'):
            if k in data:
                row = data[k][0] if data[k] else {}
                break
        if not row:
            row = data
    else:
        row = {}
        count = 'unknown'
    print(os.path.basename(f), count, list(row.keys()))
    # Print a sample record
    print('  SAMPLE:', json.dumps(row, default=str)[:400])
    print()
