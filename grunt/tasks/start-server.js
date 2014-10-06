module.exports = function (grunt) {

  grunt.registerTask('startServer',
      'start server', function () {
    var
      server_path = grunt.config('server_path'),
      os = grunt.config('os'),
      string = require('string-formatter'),
      config = grunt.config('config'),
      tasks = [],
      start,
      pwd;

    grunt.log.writeln('server_path = "%s"', server_path);

    if (os === 'win') {

      start = '{0} start-process ' +
        '-NoNewWindow ' + 
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath node ' +
        '-ArgumentList \' server.js \' ';
      start = string.format(start, config.win.powershell);
      pwd = string.format('{0} pwd',config.win.powershell);

      tasks.push(pwd);
      tasks.push(start);

      grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
      grunt.config('shell.exec.options.execOptions.cwd', '<%= server_path %>');
      grunt.config('shell.exec.command', tasks.join('&&'));
      grunt.task.run('shell');

    } else {

      grunt.config('shell.exec.options.execOptions.cwd', '<%= server_path %>');
      grunt.config('shell.exec.command', ['pwd', 'node server.js'].join('&&'));
      grunt.task.run('shell');
    }

  });
};


