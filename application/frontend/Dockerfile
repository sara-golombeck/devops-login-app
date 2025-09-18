# # Multi-stage build for React app
# FROM node:18-alpine AS builder

# WORKDIR /app

# # Copy package files
# COPY package*.json ./

# # Install dependencies
# RUN npm ci --only=production --silent

# # Copy source code
# COPY . .

# # Build the app
# RUN npm run build

# # Verify build output
# RUN ls -la build/ && test -d build/static

# # Final stage - minimal image with build output
# FROM alpine:latest AS artifacts
# COPY --from=builder /app/build /build
# CMD ["sh"]

# Multi-stage Dockerfile for React App with S3 Deployment
# This is the BEST PRACTICE version - used by Netflix, Spotify, etc.

# Stage 1: Build the React application
FROM node:18-alpine AS builder

# Set working directory
WORKDIR /app

# Copy package files first (for better caching)
COPY package*.json ./


# Install dependencies (npm ci is faster and more reliable than npm install)
RUN npm ci --silent

# Copy source code
COPY . .

# Build the production bundle
RUN npm run build

# Verify build succeeded
RUN test -d build/static || (echo "Build failed!" && exit 1)

# Stage 2: Create minimal artifacts container
# Using alpine (not scratch) so docker cp works properly
FROM alpine:latest AS artifacts

# Copy build artifacts from builder stage
COPY --from=builder /app/build /build

# Add a default command (required for docker create)
CMD ["sh"]

# Stage 3: Nginx for E2E testing
FROM nginx:alpine AS nginx
COPY --from=builder /app/build /usr/share/nginx/html
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]