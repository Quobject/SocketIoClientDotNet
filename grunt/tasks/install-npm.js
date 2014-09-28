module.exports = function (grunt) {

  grunt.registerTask('installNpm',
      'install node modules', function () {
    var
      string = require('string-formatter'),
      server_path2 = grunt.config('server_path'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      win_pwd_command = string.format('{0} pwd', config.win.powershell);

    grunt.log.writeln('server_path = "%s"', server_path2);
    grunt.log.writeln('win_pwd_command = "%s"', win_pwd_command);

    if (os === 'win') {
      grunt.config('shell.exec.options.execOptions.cwd', '<%= server_path %>');

      grunt.config('shell.exec.command', [win_pwd_command,
        'npm install'].join('&&'));
      grunt.task.run('shell');

    } else {

      grunt.config('shell.exec.options.execOptions.cwd', '<%= server_path %>');
      grunt.config('shell.exec.command', ['pwd', 'npm install'].join('&&'));
      grunt.task.run('shell');
    }

  });
};


