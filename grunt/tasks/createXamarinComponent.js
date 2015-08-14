module.exports = function (grunt) {

  grunt.registerTask('createXamarinComponent',
     'create component ', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      xamarin_component = os === 'win' ? config.win.xamarin_component : config.linux.xamarin_component,
      working_path = grunt.config('working_path'),
      package_path = working_path + '/Component/',
      configuration = grunt.config('msbuild_configuration'),
      template_file_content,
      tasks = [];

    if (configuration !== 'Release') {
      grunt.log.writeln('wrong configuration = ' + configuration);
      return;
    }


    if (! fs.existsSync(working_path)) {
      fs.mkdirSync(working_path);
      fs.mkdirSync(package_path);
    }
    if (!fs.existsSync(package_path)) {
      fs.mkdirSync(package_path);
    }


    template_file_content = fs.readFileSync('./templates/SocketIoClientDotNet.yaml');
    template_file_content = S(template_file_content).replaceAll('@VERSION@', config.version).s;
    fs.writeFileSync(string.format('{0}component.yaml', package_path), template_file_content);


    tasks.push('echo %cd%');
    tasks.push(string.format('{0} package', xamarin_component));
    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.options.execOptions.cwd', package_path);
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');       
  });
};