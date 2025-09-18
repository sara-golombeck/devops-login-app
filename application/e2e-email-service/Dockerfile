# # tests/integration/Dockerfile
# FROM python:3.11-slim

# WORKDIR /app

# # Copy requirements and install dependencies
# COPY requirements.txt .
# RUN pip install --no-cache-dir -r requirements.txt

# # Copy test files
# COPY . .

# # Default command
# CMD ["pytest", "test_integration.py", "-v", "--tb=short"]


FROM python:3.11-slim

WORKDIR /app

# Copy requirements first
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy only test files
COPY test_*.py ./
COPY *.py ./

# Default command
CMD ["pytest", "test_integration.py", "-v", "--tb=short"]