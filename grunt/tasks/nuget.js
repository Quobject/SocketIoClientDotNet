module.exports = function (grunt) {

  grunt.registerTask('nuget',
    'get nuget assemblies', function () {
    var
      //fs = require('fs'),
      //S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      //configuration = grunt.config('msbuild_configuration'),
      nuget_builds = grunt.config('nuget_builds'),
      nuget_path = os === 'win' ?
        config.win.nuget : config.linux.nuget,
      format_str = os === 'win' ?
        '{0} restore "{1}"' :
        'mono --runtime=v4.0.30319 {0} restore {1}',
      tasks = [],
      i;

    function restorePackagesWithTitle(title) {
      var
        sln = string.format('{0}/../../Src/{1}/{2}.sln',__dirname, title,title),
        restore = string.format(format_str, nuget_path, sln);

      tasks.push(restore);
    }

    if (os === 'win') {
      for (i = 0; i < nuget_builds.length; i++) {
        restorePackagesWithTitle(nuget_builds[i].Name);
      }
    }

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');
  });
};

