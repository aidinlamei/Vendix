window.vendix = window.vendix || {};

window.vendix.cart = {
    load: function () {
        try {
            return window.localStorage.getItem('vendix.cart') || '';
        } catch (e) {
            return '';
        }
    },
    save: function (json) {
        try {
            window.localStorage.setItem('vendix.cart', json);
        } catch (e) {
        }
    },
    clear: function () {
        try {
            window.localStorage.removeItem('vendix.cart');
        } catch (e) {
        }
    }
};

window.vendix.buyer = {
    load: function () {
        try {
            return window.localStorage.getItem('vendix.buyerId') || '';
        } catch (e) {
            return '';
        }
    },
    save: function (id) {
        try {
            window.localStorage.setItem('vendix.buyerId', id);
        } catch (e) {
        }
    }
};
