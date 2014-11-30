fromAll() 
.when({
    GameScheduled : function(s, e) {
        linkTo('Proj-GamesList', e);
        return s;
    },
    GameJoined: function (s, e) {
        linkTo('Proj-GamesList', e);
        return s;
    },
    GameAbandonned: function (s, e) {
        linkTo('Proj-GamesList', e);
        return s;
    }
})