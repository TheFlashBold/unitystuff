const Koa = require('koa');
const Router = require('koa-router');
const md5 = require('md5');
const Promise = require('bluebird');
const uuidv1 = require('uuid/v1');
const socketio = require('socket.io');

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
    project: {
        type: String
    },
    data: {
        type: mongoose.Schema.Types.Mixed
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
                password: auth.password,
                project: auth.project || ""
            });

            return user.save().then(() => {
                return ctx.body = {
                    success: true,
                    error: ""
                }
            });
        });
    }
    else {
        console.log("register failed");
        return ctx.body = {
            success: false,
            error: "register failed"
        }
    }
});

router.post('/login', (ctx, next) => {
	let auth = ctx.request.body;

	let query = {
	    username: auth.username,
        password: auth.password
	};

	if(auth.project){
	    query.project = auth.project;
    }

    return UserModel.findOne(query).exec().then(user => {
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

router.post('/save', (ctx, next) => {
    let auth = ctx.request.body;
    if(auth.id && auth.session){
        return UserModel.findOne({_id: mongoose.Types.ObjectId(auth.id), session: auth.session}).exec().then(user => {
            if(user){
                user.set('data', JSON.parse(auth.data));
                user.markModified('data');
                user.save();
                return ctx.body = {
                    success: true,
                    error: ""
                };
            }
            ctx.body = {
                success: false,
                error: "No user found."
            }
        });
    }
    else {
        ctx.body = {
            success: false,
            error: ""
        }
    }
});

app.use(koaBody());
app.use(router.routes());
app.use(router.allowedMethods());

let http = app.listen(5000);

/* Maybe for laterrrrr
const io = socketio.listen(http);

io.on('connection', socket => {
    socket.emit('hi', 'test');
    socket.on('hi', data => {
        console.log(data);
    });
});

*/