#include <iostream>
#include <chrono>
#include <cmath>


#if defined(_MSC_VER)
    //  Microsoft 
    #ifdef BUILD
      #define EXPORT __declspec(dllexport)
    #else
      #define EXPORT
    #endif
    #define IMPORT __declspec(dllimport)
#elif defined(__GNUC__)
    //  GCC
    #ifdef BUILD
      #define EXPORT __attribute__((visibility("default")))
    #else
      #define EXPORT
    #endif
    #define IMPORT
#else
    //  do nothing and hope for the best?
    #define EXPORT
    #define IMPORT
    #pragma warning Unknown dynamic link import/export semantics.
#endif





EXPORT double* FakePricing(int nbInputElement,
		    double *input,
		    int workLoadTimeInMs,
		    int nbOutputElement)
{
  if (nbInputElement <= 0 || nbOutputElement <= 0)
  {
    std::cout << "Cannot execute function with nb element <= 0" << std::endl;
    return nullptr;
  }

  if (workLoadTimeInMs <= 0)
  {
      workLoadTimeInMs = 1;
  }

  double *output = new double[nbOutputElement];
  
  // Record start time
  auto start = std::chrono::high_resolution_clock::now();
  auto finish = start;
  auto elapsed = finish - start;
  int r_idx = 0;

  double result = 0.0;
  for (int i = 0; i < nbOutputElement; i++)
  {
    result += std::pow(input[i], 3.0);
  }
  
  while (elapsed.count() < workLoadTimeInMs)
  {
    output[r_idx] = result / (double)nbOutputElement;

    r_idx++;
    
    if (r_idx >= nbOutputElement)
      r_idx = 0;
    
    elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::high_resolution_clock::now() - start);
  }
  
  return output;
}
