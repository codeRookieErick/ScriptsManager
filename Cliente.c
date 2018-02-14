#include "Client.h"

#ifdef __cplusplus
extern "C"{
#endif

JNIEXPORT jint JNICALL Java_Client_MakeATest
  (JNIEnv *env, jclass object){
	
	  return 3+3;
  }
  
#ifdef __cplusplus
}
#endif