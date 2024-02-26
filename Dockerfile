FROM node:20.11

WORKDIR /usr/src/app
COPY . /usr/src/app

RUN npm install

EXPOSE 3000

CMD ["npm", "run", "dev"]
