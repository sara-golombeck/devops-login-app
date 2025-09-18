#!/usr/bin/env python3
"""
Integration tests for EmailService API
Modern pytest-based implementation
"""

import os
import time
from typing import Dict, Any

import pytest
import requests
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry


class TestEmailServiceAPI:
    """Integration tests for EmailService API endpoints"""
    
    @pytest.fixture(scope="class")
    def api_client(self) -> requests.Session:
        """Configure HTTP client with retries and timeouts"""
        session = requests.Session()
        
        # Configure retry strategy
        retry_strategy = Retry(
            total=3,
            backoff_factor=1,
            status_forcelist=[429, 500, 502, 503, 504],
        )
        adapter = HTTPAdapter(max_retries=retry_strategy)
        session.mount("http://", adapter)
        session.mount("https://", adapter)
        
        # Set default timeout
        session.timeout = 10
        
        return session
    
    @pytest.fixture(scope="class")
    def base_url(self) -> str:
        """Get base URL from environment"""
        return os.getenv('BASE_URL', 'http://frontend:8080')
    
    @pytest.fixture(scope="class", autouse=True)
    def wait_for_service(self, api_client: requests.Session, base_url: str):
        """Wait for service to be ready before running tests"""
        max_attempts = 30
        for attempt in range(max_attempts):
            try:
                response = api_client.get(f"{base_url}/api/health")
                if response.status_code == 200:
                    return
            except requests.exceptions.RequestException:
                pass
            
            if attempt < max_attempts - 1:
                time.sleep(2)
        
        pytest.fail("Service failed to start within timeout")
    
    def test_health_endpoint(self, api_client: requests.Session, base_url: str):
        """Test health check endpoint returns correct status"""
        response = api_client.get(f"{base_url}/api/health")
        
        assert response.status_code == 200
        data = response.json()
        assert data["status"] == "Healthy"
        assert "timestamp" in data
    
    @pytest.mark.parametrize("email,expected_status,expected_success", [
        ("test@example.com", 200, True),
        ("user@domain.org", 200, True),
    ])
    def test_login_valid_emails(
        self, 
        api_client: requests.Session, 
        base_url: str,
        email: str,
        expected_status: int,
        expected_success: bool
    ):
        """Test login with valid email formats"""
        payload = {"email": email}
        response = api_client.post(
            f"{base_url}/api/auth/login",
            json=payload,
            headers={"Content-Type": "application/json"}
        )
        
        # E2E tests must expect success for valid emails
        assert response.status_code == expected_status, \
            f"Login failed for {email}. Status: {response.status_code}, Response: {response.text}"
        
        data = response.json()
        assert data["success"] is expected_success, \
            f"Unexpected success value for {email}. Expected: {expected_success}, Got: {data.get('success')}"
    
    @pytest.mark.parametrize("invalid_email", [
        "invalid-email",
        "no-at-sign",
        "@domain.com",
        "user@",
        "",
    ])
    def test_login_invalid_emails(
        self, 
        api_client: requests.Session, 
        base_url: str,
        invalid_email: str
    ):
        """Test login with invalid email formats"""
        payload = {"email": invalid_email}
        response = api_client.post(
            f"{base_url}/api/auth/login",
            json=payload,
            headers={"Content-Type": "application/json"}
        )
        
        assert response.status_code == 400
        data = response.json()
        assert data["success"] is False
    
    def test_api_performance(self, api_client: requests.Session, base_url: str):
        """Test API response time is acceptable"""
        start_time = time.time()
        response = api_client.get(f"{base_url}/api/health")
        duration = time.time() - start_time
        
        assert response.status_code == 200
        assert duration < 2.0, f"API response too slow: {duration:.2f}s"


if __name__ == "__main__":
    # Allow running directly for debugging
    pytest.main([__file__, "-v"])