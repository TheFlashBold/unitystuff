const Koa = require('koa');
const Router = require('koa-router');

var app = new Koa();
var router = new Router();

const koaBody = require('koa-body');


router.post('/login', (ctx, next) => {
	let auth = ctx.request.body.auth;
	
	if(auth && auth.username === "tycho" && password === "tycho"){
		ctx.body = {
			success: true
		};
	} else {
		ctx.body = {
			success: false
		};
	}
});

app.use(koaBody());
app.use(router.routes());
app.use(router.allowedMethods());

app.listen(3000);
