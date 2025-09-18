#!/usr/bin/env python3
"""
Frontend integration tests for static files served from CloudFront
"""

import os
import pytest
import requests
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry


class TestFrontendStatic:
    """Test static files are served correctly from CloudFront"""
    
    @pytest.fixture(scope="class")
    def http_client(self) -> requests.Session:
        """Configure HTTP client"""
        session = requests.Session()
        retry_strategy = Retry(total=3, backoff_factor=1)
        adapter = HTTPAdapter(max_retries=retry_strategy)
        session.mount("http://", adapter)
        session.mount("https://", adapter)
        session.timeout = 10
        return session
    
    @pytest.fixture(scope="class")
    def static_files_url(self) -> str:
        """Get static files URL from environment"""
        return os.getenv('STATIC_FILES_URL', 'https://localhost')
    
    def test_index_html_loads(self, http_client: requests.Session, static_files_url: str):
        """Test main HTML file loads successfully"""
        response = http_client.get(f"{static_files_url}/")
        
        assert response.status_code == 200
        assert "text/html" in response.headers.get("content-type", "")
        assert "<title>" in response.text
    
    def test_static_assets_have_cache_headers(self, http_client: requests.Session, static_files_url: str):
        """Test static assets have proper cache headers"""
        # Try to find a JS or CSS file
        index_response = http_client.get(f"{static_files_url}/")
        content = index_response.text
        
        # Look for static file references
        import re
        js_files = re.findall(r'src="([^"]*\.js)"', content)
        css_files = re.findall(r'href="([^"]*\.css)"', content)
        
        if js_files:
            js_url = js_files[0]
            if not js_url.startswith('http'):
                js_url = f"{static_files_url}{js_url}"
            
            response = http_client.get(js_url)
            assert response.status_code == 200
            
            # Check cache headers for static files
            cache_control = response.headers.get("cache-control", "")
            assert "max-age" in cache_control.lower()
    
    def test_html_no_cache_headers(self, http_client: requests.Session, static_files_url: str):
        """Test HTML files have no-cache headers"""
        response = http_client.get(f"{static_files_url}/")
        
        cache_control = response.headers.get("cache-control", "")
        # HTML should not be cached aggressively
        assert "no-cache" in cache_control.lower() or "max-age=0" in cache_control.lower()
    
    def test_cloudfront_headers_present(self, http_client: requests.Session, static_files_url: str):
        """Test CloudFront headers are present"""
        response = http_client.get(f"{static_files_url}/")
        
        # CloudFront adds these headers
        headers = response.headers
        assert "x-cache" in headers or "via" in headers or "x-amz-cf-id" in headers


if __name__ == "__main__":
    pytest.main([__file__, "-v"])