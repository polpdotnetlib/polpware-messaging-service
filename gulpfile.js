const { src, dest, parallel } = require('gulp');
const bumpversion = require('gulp-bump');

// `fs` is used instead of require to prevent caching in watch (require caches)
const fs = require('fs');

function getVersion() {
    return fs.readFileSync('./VERSION', 'utf8', function(err, data) {
        return data;
    });
};

function bump() {

    const newVer = getVersion().trim();

// bump versions on package/bower/manifest
    return src(['./src/common.props'])
        .pipe(bumpversion({
            version: newVer
        }))
        .pipe(dest(function(x) {
            return x.base;
        }));
}

exports.bump = bump;
exports.default = bump;
