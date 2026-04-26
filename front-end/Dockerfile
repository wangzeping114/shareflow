# 基于官方 Node 镜像构建前端
FROM node:20 AS build
WORKDIR /app
COPY front-end/package.json ./
COPY front-end/pnpm-lock.yaml ./
RUN npm install -g pnpm && pnpm install
COPY front-end/ .
RUN pnpm build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
