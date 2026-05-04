# Stage 1: Build the Vue app
FROM node:lts-alpine AS build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: Serve with Nginx
FROM nginx:stable-alpine AS production-stage
# Copy the custom nginx config
COPY default.conf /etc/nginx/conf.d/default.conf 
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 443
CMD ["nginx", "-g", "daemon off;"]
