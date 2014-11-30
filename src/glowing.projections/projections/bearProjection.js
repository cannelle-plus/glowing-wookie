fromAll() 
.when({
    SignedIn: function (s, e) {
        linkTo('Proj-BearList', e);
        return s;
    }
})