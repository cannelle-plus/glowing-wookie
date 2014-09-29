fromAll() 
.when({
    GameCreated : function(s, e) {
        linkTo('Proj-GamesList', e);
        return s;
    }
})