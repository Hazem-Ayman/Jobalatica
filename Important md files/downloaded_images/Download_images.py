import os
import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse

def download_images(url, folder_name="downloaded_images"):
    if not os.path.exists(folder_name):
        os.makedirs(folder_name)

    # Adding headers to look like a real browser
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
    }

    try:
        # Added a timeout (5 seconds to connect, 10 to read)
        response = requests.get(url, headers=headers, timeout=(5, 10))
        response.raise_for_status()
    except requests.exceptions.RequestException as e:
        print(f"Error fetching the main URL: {e}")
        return

    soup = BeautifulSoup(response.text, 'html.parser')
    img_tags = soup.find_all('img')
    print(f"Found {len(img_tags)} images.")

    for img in img_tags:
        img_url = img.get('src')
        if not img_url:
            continue

        img_url = urljoin(url, img_url)
        filename = os.path.basename(urlparse(img_url).path)
        if not filename:
            continue

        filepath = os.path.join(folder_name, filename)

        try:
            # Added timeout here as well to prevent hanging on a single bad image link
            img_response = requests.get(img_url, headers=headers, timeout=5)
            img_response.raise_for_status()
            
            with open(filepath, 'wb') as f:
                f.write(img_response.content)
            print(f"Saved: {filename}")
            
        except KeyboardInterrupt:
            print("\nStopped by user.")
            return
        except Exception as e:
            print(f"Skipping {img_url}: {e}")


target_url = "https://www.tavus.io/" 
download_images(target_url)