import os
import zipfile
import shutil

def zipdir(path, ziph):
    # ziph is zipfile handle
    for root, dirs, files in os.walk(path):
        for file in files:
            ziph.write(os.path.join(root, file))

if __name__=='__main__':

	#create local directory
	if os.path.isdir("ezIOmeter"):
		shutil.rmtree("ezIOmeter")	
	os.makedirs("ezIOmeter")

	#make some necessary directories under the root directory	
	os.makedirs("ezIOmeter/Results")	
	
	#copy files and directories to the root directory
	shutil.copy("../bin/Release/ezIOmeter.exe","ezIOmeter/ezIOmeter.exe")
	shutil.copy("../bin/Release/ezIOmeter_Lib.dll","ezIOmeter/ezIOmeter_Lib.dll")
	shutil.copy("../bin/Release/settings.conf","ezIOmeter/settings.conf")
	shutil.copy("../bin/Release/MathNet.Numerics.dll","ezIOmeter/MathNet.Numerics.dll")
	shutil.copy("../bin/Release/MathNet.Numerics.xml","ezIOmeter/MathNet.Numerics.xml")
	shutil.copy("../Documentation/ezIOmeter_User_Guide.pdf","ezIOmeter/ezIOmeter_User_Guide.pdf")
	shutil.copy("README.txt","ezIOmeter/README.TXT")
	shutil.copytree("../bin/Release/IOmeter","ezIOmeter/IOmeter")
	shutil.copytree("../bin/Release/IOmeterConfigFiles","ezIOmeter/IOmeterConfigFiles")
	shutil.copytree("Support","ezIOmeter/Support")
		
	#create zip file
	if os.path.exists("ezIOmeter.zip"):
		os.remove("ezIOmeter.zip")
	zipf = zipfile.ZipFile('ezIOmeter.zip','w')
	zipdir('ezIOmeter',zipf)	
	zipf.close()