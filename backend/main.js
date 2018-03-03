const Koa = require('koa');
const Router = require('koa-router');
const md5 = require('md5');
const Promise = require('bluebird');
const uuidv1 = require('uuid/v1');

const mongoose = require('mongoose');
mongoose.connect('mongodb://localhost/unitybackend');

const UserModel = mongoose.model('user', {
    username: {
        type: String,
        required: true
    },
    password: {
        type: String,
        required: true
    },
    session: {
        type: String
    },
    data: {
        type: String
    }
});


let app = new Koa();
let router = new Router();

const koaBody = require('koa-body');

router.get('/', (ctx, next) => {
	ctx.body = "yeeeh";
});

router.post('/register', (ctx, next) => {
    let auth = ctx.request.body;
    if(auth.username && auth.password === auth.passwordrepeat){
        return UserModel.findOne({username: auth.username, password: auth.password}).exec().then(existingUser => {
            if(existingUser){
                return ctx.body = {
                    success: false,
                    error: "User already exists"
                };
            }
            console.log("register started");
            let user = new UserModel({
                username: auth.username,
                password: auth.password
            });

            return user.save().then(() => {
                ctx.body = {
                    success: true,
                    error: ""
                }
            });
        });
    }
    else {
        console.log("register failed");
        ctx.body = {
            success: true,
            error: ""
        }
    }
});

router.post('/login', (ctx, next) => {
	let auth = ctx.request.body;

    return UserModel.findOne({username: auth.username, password: auth.password}).exec().then(user => {
        if(!user){
            console.log("No user found for '" + auth.username + "' '" + auth.password + "'");
            ctx.body = {
                id: "",
                session: "",
                data: "",
                success: false,
                error: "wrong password / user doesn't exist"
            };
            return;
        }

        console.log("User found for '" + auth.username + "' '" + auth.password + "'");
        let session = uuidv1();
        user.set('session', session);
        user.save();
        ctx.body = {
            id: user._id,
            session: session,
            data: user.data,
            success: true,
            error: ""
        };
    });
});

app.use(koaBody());
app.use(router.routes());
app.use(router.allowedMethods());

app.listen(5000);
