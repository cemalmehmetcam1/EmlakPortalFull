function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(decodeURIComponent(window.atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join('')));
}

function isAdmin() {
    var token = localStorage.getItem("token");
    if (!token) return false;
    var payload = parseJwt(token);
    var role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    return role === "Admin";
}

// Tüm AJAX isteklerine otomatik token ekleme
$.ajaxSetup({
    beforeSend: function (xhr) {
        var token = localStorage.getItem("token");
        if (token) xhr.setRequestHeader("Authorization", "Bearer " + token);
    }
});